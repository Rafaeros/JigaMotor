# Guia de Geração de Chaves e Testes LoRaWAN

Este documento detalha os procedimentos técnicos para a geração de identificadores únicos e a execução dos testes de emergência para os dispositivos IoTag, baseando-se na implementação atual do sistema Jiga-MQTT.

---

## 1. Geração de Chaves e Identificadores

A segurança e a unicidade dos dispositivos dependem da geração aleatória de chaves criptográficas e do controle sequencial de IDs de hardware.

### Chaves Criptográficas (LoRaWAN)
Geradas via `core/databases/devices.py` usando a biblioteca `secrets` do Python para garantir aleatoriedade criptográfica.

| Campo | Função | Origem do Dado | Formato |
| :--- | :--- | :--- | :--- |
| **DEVADDR** | Endereço do dispositivo na rede | `secrets.token_hex(4)` | 8 hex chars (4 bytes) |
| **DEVEUI** | Identificador único do dispositivo | `secrets.token_hex(8)` | 16 hex chars (8 bytes) |
| **APPEUI** | Identificador da aplicação | `secrets.token_hex(8)` | 16 hex chars (8 bytes) |
| **NWSKEY** | Chave de sessão de rede | `secrets.token_hex(16)` | 32 hex chars (16 bytes) |
| **APPSKEY** | Chave de sessão de aplicação | `secrets.token_hex(16)` | 32 hex chars (16 bytes) |

### Identificadores de Hardware e Comunicação
| Campo | Descrição | Lógica de Geração |
| :--- | :--- | :--- |
| **LORA ID** | ID sequencial de produção | Obtido do SharePoint (Item ID 154), incrementado a cada novo dispositivo. |
| **SÉRIE** | Número de série comercial | Composto pelo prefixo `108` + (13.000.000 + LoraID). |
| **BLE MAC** | Endereço MAC Bluetooth | `secrets.randbits(22) \| 12582912` (Gera um valor de 24 bits com prefixo fixo). |
| **BLUETOOTH**| Nome/ID de pareamento BLE | Combinação do `BLE MAC` + final do `LORA ID`. |

---

## 2. Protocolo de Testes LoRaWAN (Emergência)

Os testes de emergência validam a comunicação bidirecional entre o Network Server (Everynet) e o dispositivo final.

### Fluxo de Teste: Entrar em Emergência
1.  **Comando de Rede:** O sistema publica via MQTT um payload de downlink no tópico do dispositivo.
    *   **Payload:** `1wEB` (Comando: `CMD_EMERGENCY_ON`).
2.  **Monitoramento BLE:** A Jiga escuta os pacotes de *Advertisement* do dispositivo.
    *   **Critério de Sucesso:** O bit de status de emergência no pacote BLE deve mudar para `1`.
3.  **Monitoramento Serial:** Caso disponível, o log serial deve confirmar a transição de estado.

### Fluxo de Teste: Sair de Emergência
1.  **Comando de Rede:** Publicação do payload de encerramento.
    *   **Payload:** `1wEA` (Comando: `CMD_EMERGENCY_OFF`).
2.  **Monitoramento BLE:**
    *   **Critério de Sucesso:** O bit de status de emergência no pacote BLE deve retornar para `0`.
3.  **Persistência e Bloqueio:** Após a saída, o dispositivo é marcado como testado no SharePoint e suas tags são atualizadas no Everynet para produção.

---

## 3. Resumo de Integridade (Cross-Check)

Para evitar duplicidade ou dados órfãos, o sistema realiza as seguintes validações:
1.  **Lock de Série:** O LoraID é reservado no SharePoint antes de qualquer outra operação.
2.  **Retry Everynet:** Se a criação no Everynet falhar (por duplicidade de DevEUI), o sistema descarta as chaves e gera um novo set aleatório (até 10 tentativas).
3.  **Conformidade de Base:** O estado final deve ser idêntico em três locais: **Memória Local da Jiga == Banco SharePoint == Registro Everynet**.
