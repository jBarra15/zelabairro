/*
 * ZelaBairro — Lixeira inteligente (ESP32 no Wokwi)
 * Joel — RM 560796 — 3ESPS
 *
 * O sensor ultrassônico mede o nível de lixo. Quando a lixeira passa de 80% por
 * 3 leituras seguidas, o ESP32 ABRE UMA OCORRÊNCIA na API ZelaBairro
 * (POST /api/ocorrencias, origem "Sensor"). Ela aparece no terminal da subprefeitura
 * e no app dos moradores, sem ninguém precisar reclamar.
 * Quando o gari esvazia (nível < 30%) ou aperta "Coleta realizada", o ciclo reinicia.
 */
#include <WiFi.h>
#include <HTTPClient.h>
#include <WiFiClientSecure.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>

// ---------------- Configuração ----------------
const char* WIFI_SSID = "Wokwi-GUEST";
const char* WIFI_SENHA = "";

// Endereço público da API (aba "Ports" do VS Code, porta 5080, visibilidade Public).
// Exemplo: "https://abc123-5080.brs.devtunnels.ms"   (sem barra no final)
const char* API_URL = "https://COLE-AQUI-O-ENDERECO-5080.brs.devtunnels.ms";

// Dados fixos desta lixeira (como seria no cadastro do equipamento)
const char* LIXEIRA_NOME = "Lixeira #07";
const char* LIXEIRA_ENDERECO = "Lixeira #07 - Av. Paulista, 1106";
const int BAIRRO_ID = 1;        // Bela Vista
const int CATEGORIA_LIXO = 1;   // Lixo e descarte irregular
const float LATITUDE = -23.5614;
const float LONGITUDE = -46.6559;

const float ALTURA_LIXEIRA_CM = 100.0;  // distância do sensor (tampa) até o fundo
const int NIVEL_ALERTA = 80;            // % para abrir ocorrência
const int NIVEL_ESVAZIADA = 30;         // % abaixo do qual consideramos que houve coleta
const int LEITURAS_CONFIRMACAO = 3;     // evita alarme falso (ex.: mão passando na frente)
const unsigned long INTERVALO_LEITURA_MS = 1000;
const unsigned long INTERVALO_REENVIO_MS = 30000;

// ---------------- Pinos ----------------
const int PINO_TRIG = 5;
const int PINO_ECHO = 18;
const int LED_VERDE = 25;
const int LED_AMARELO = 26;
const int LED_VERMELHO = 27;
const int PINO_BOTAO = 4;
const int PINO_BUZZER = 19;

LiquidCrystal_I2C lcd(0x27, 16, 2);

// ---------------- Estado ----------------
int leiturasAcimaDoLimite = 0;
bool ocorrenciaAberta = false;   // já avisamos a prefeitura neste ciclo?
bool envioPendente = false;      // passou do limite mas a API não respondeu
int ocorrenciaId = 0;
unsigned long ultimaLeitura = 0;
unsigned long ultimaTentativa = 0;

// ============================================================
float medirDistanciaCm() {
  digitalWrite(PINO_TRIG, LOW);
  delayMicroseconds(2);
  digitalWrite(PINO_TRIG, HIGH);
  delayMicroseconds(10);
  digitalWrite(PINO_TRIG, LOW);
  unsigned long duracao = pulseIn(PINO_ECHO, HIGH, 30000);  // timeout ~5 m
  if (duracao == 0) return -1;                               // sem eco: leitura inválida
  return duracao * 0.0343 / 2.0;
}

int calcularNivel(float distanciaCm) {
  float nivel = (ALTURA_LIXEIRA_CM - distanciaCm) / ALTURA_LIXEIRA_CM * 100.0;
  return constrain((int)round(nivel), 0, 100);
}

void atualizarLeds(int nivel) {
  digitalWrite(LED_VERDE, nivel < 50);
  digitalWrite(LED_AMARELO, nivel >= 50 && nivel < NIVEL_ALERTA);
  digitalWrite(LED_VERMELHO, nivel >= NIVEL_ALERTA);
}

void mostrar(int nivel, const String& status) {
  lcd.setCursor(0, 0);
  String linha1 = String(LIXEIRA_NOME) + "  " + String(nivel) + "%";
  while (linha1.length() < 16) linha1 += " ";
  lcd.print(linha1.substring(0, 16));
  lcd.setCursor(0, 1);
  String linha2 = status;
  while (linha2.length() < 16) linha2 += " ";
  lcd.print(linha2.substring(0, 16));
}

void bip(int vezes) {
  for (int i = 0; i < vezes; i++) {
    tone(PINO_BUZZER, 1200, 120);
    delay(200);
  }
}

// ============================================================
bool conectarWifi() {
  if (WiFi.status() == WL_CONNECTED) return true;
  Serial.print("Conectando ao Wi-Fi");
  WiFi.begin(WIFI_SSID, WIFI_SENHA, 6);
  unsigned long inicio = millis();
  while (WiFi.status() != WL_CONNECTED && millis() - inicio < 10000) {
    delay(250);
    Serial.print(".");
  }
  Serial.println(WiFi.status() == WL_CONNECTED ? " conectado!" : " falhou.");
  return WiFi.status() == WL_CONNECTED;
}

/** Extrai o "id" da resposta JSON da API, sem biblioteca externa. */
int extrairId(const String& json) {
  int pos = json.indexOf("\"id\":");
  if (pos < 0) return 0;
  return json.substring(pos + 5).toInt();
}

/** Abre a ocorrência na API. Devolve o id criado, ou 0 se falhou. */
int abrirOcorrencia(int nivel) {
  if (!conectarWifi()) return 0;

  String corpo = String("{") +
    "\"titulo\":\"Lixeira cheia (" + String(nivel) + "%) - " + LIXEIRA_NOME + "\"," +
    "\"descricao\":\"Aberta automaticamente pelo sensor ultrassonico. Nivel medido: " + String(nivel) + "%.\"," +
    "\"categoriaId\":" + String(CATEGORIA_LIXO) + "," +
    "\"bairroId\":" + String(BAIRRO_ID) + "," +
    "\"moradorId\":null," +
    "\"latitude\":" + String(LATITUDE, 6) + "," +
    "\"longitude\":" + String(LONGITUDE, 6) + "," +
    "\"endereco\":\"" + LIXEIRA_ENDERECO + "\"," +
    "\"origem\":\"Sensor\"}";

  String url = String(API_URL) + "/api/ocorrencias";
  Serial.println("POST " + url);
  Serial.println(corpo);

  HTTPClient http;
  WiFiClientSecure clienteSeguro;
  WiFiClient clienteSimples;
  bool https = url.startsWith("https");
  if (https) {
    clienteSeguro.setInsecure();  // simulação: não validamos o certificado do túnel
    http.begin(clienteSeguro, url);
  } else {
    http.begin(clienteSimples, url);
  }
  http.setTimeout(10000);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("X-Tunnel-Skip-AntiPhishing-Page", "true");  // túnel do VS Code

  int status = http.POST(corpo);
  String resposta = status > 0 ? http.getString() : http.errorToString(status);
  http.end();

  Serial.printf("Resposta HTTP %d: %s\n", status, resposta.c_str());
  return status == 201 ? extrairId(resposta) : 0;
}

void tentarAvisar(int nivel) {
  ultimaTentativa = millis();
  mostrar(nivel, "Avisando...");
  int id = abrirOcorrencia(nivel);
  if (id > 0) {
    ocorrenciaAberta = true;
    envioPendente = false;
    ocorrenciaId = id;
    bip(2);
    Serial.printf(">> Ocorrencia #%d aberta na prefeitura.\n", id);
  } else {
    envioPendente = true;
    Serial.println(">> API indisponivel. Nova tentativa em 30 s.");
  }
}

void reiniciarCiclo(const char* motivo) {
  if (ocorrenciaAberta || envioPendente) Serial.printf(">> Ciclo reiniciado: %s\n", motivo);
  ocorrenciaAberta = false;
  envioPendente = false;
  ocorrenciaId = 0;
  leiturasAcimaDoLimite = 0;
}

// ============================================================
void setup() {
  Serial.begin(115200);
  pinMode(PINO_TRIG, OUTPUT);
  pinMode(PINO_ECHO, INPUT);
  pinMode(LED_VERDE, OUTPUT);
  pinMode(LED_AMARELO, OUTPUT);
  pinMode(LED_VERMELHO, OUTPUT);
  pinMode(PINO_BOTAO, INPUT_PULLUP);
  pinMode(PINO_BUZZER, OUTPUT);

  lcd.init();
  lcd.backlight();
  lcd.print("ZelaBairro IoT");
  lcd.setCursor(0, 1);
  lcd.print("Conectando...");

  conectarWifi();
  Serial.println("ZelaBairro - lixeira inteligente pronta. Arraste o sensor para simular o lixo.");
}

void loop() {
  // Botão "Coleta realizada" (o gari confirma que esvaziou)
  if (digitalRead(PINO_BOTAO) == LOW) {
    reiniciarCiclo("coleta confirmada pelo botao");
    mostrar(0, "Coleta OK!");
    bip(1);
    delay(1500);
  }

  if (millis() - ultimaLeitura < INTERVALO_LEITURA_MS) return;
  ultimaLeitura = millis();

  float distancia = medirDistanciaCm();
  if (distancia < 0) {
    mostrar(0, "Erro no sensor");
    Serial.println("Leitura invalida (sem eco).");
    return;
  }

  int nivel = calcularNivel(distancia);
  atualizarLeds(nivel);
  Serial.printf("Distancia: %.1f cm | Nivel: %d%%\n", distancia, nivel);

  // Esvaziou: nova coleta detectada pelo próprio sensor
  if (nivel < NIVEL_ESVAZIADA && (ocorrenciaAberta || envioPendente)) {
    reiniciarCiclo("nivel caiu (lixeira esvaziada)");
  }

  if (nivel >= NIVEL_ALERTA) {
    leiturasAcimaDoLimite++;
  } else {
    leiturasAcimaDoLimite = 0;
  }

  bool confirmadoCheia = leiturasAcimaDoLimite >= LEITURAS_CONFIRMACAO;

  if (confirmadoCheia && !ocorrenciaAberta) {
    bool primeiraTentativa = !envioPendente;
    bool horaDeReenviar = envioPendente && millis() - ultimaTentativa >= INTERVALO_REENVIO_MS;
    if (primeiraTentativa || horaDeReenviar) tentarAvisar(nivel);
  }

  // Status no display
  if (ocorrenciaAberta)        mostrar(nivel, "Ocorrencia #" + String(ocorrenciaId));
  else if (envioPendente)      mostrar(nivel, "API offline");
  else if (nivel >= NIVEL_ALERTA) mostrar(nivel, "CHEIA! " + String(leiturasAcimaDoLimite) + "/" + String(LEITURAS_CONFIRMACAO));
  else if (nivel >= 50)        mostrar(nivel, "Quase cheia");
  else                         mostrar(nivel, "OK");
}
