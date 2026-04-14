using System.Diagnostics;
using System.Text;


namespace JigaMotor.Features.nRF54L15.GravarPlaca;

public record GravarPlacaRequest(string DevEui, string AppEui, string AppKey, string CaminhoHex);
public record GravarPlacaResponse(string Status, string Mensagem);

public class GravarPlacaUseCase 
{
    // Recuado para 0x0017C000 para respeitar o limite físico de 1524 KB do nRF54L15
    private const uint EnderecoLorawan = 0x0017C000;

    // Nome do arquivo temporário do nosso "Cavalo de Troia"
    private const string CaminhoHexChaves = "chaves_lorawan_temp.hex";

    public async Task<GravarPlacaResponse> ExecutarAsync(GravarPlacaRequest request)
    {
        try
        {
            // 1. Transforma as strings hexadecimais em bytes puros
            byte[] payload = MontarPayloadLorawan(request.DevEui, request.AppEui, request.AppKey);

            // 2. GERA O ARQUIVO .HEX NATIVAMENTE (Com endereço embutido)
            await GerarArquivoIntelHexAsync(payload, EnderecoLorawan, CaminhoHexChaves);

            // 3. Roda o motor externo
            await Task.Run(() =>
            {
                Console.WriteLine("🔌 Destrancando e Apagando o chip (Recover)...");
                RodarComandoNrfjprog("--family NRF54L --recover");

                Console.WriteLine($"🔑 Injetando o .hex das chaves LoRaWAN no endereço {EnderecoLorawan:X}...");
                // Note que o --baseaddress sumiu! O J-Link lê o endereço de dentro do .hex
                RodarComandoNrfjprog($"--family NRF54L --program {CaminhoHexChaves} --sectorerase");
                Console.WriteLine("✅ Chaves injetadas com sucesso!");

                Console.WriteLine("📦 Gravando o Firmware do Sistema Operacional...");
                RodarComandoNrfjprog($"--family NRF54L --program {request.CaminhoHex} --verify --reset");
                Console.WriteLine("✅ SO gravado e placa reiniciada!");
            });

            return new GravarPlacaResponse("sucesso", "Placa fabricada e trancada com sucesso.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Falha de Manufatura: {ex.Message}");
        }
        finally
        {
            // Limpeza do disco
            if (File.Exists(CaminhoHexChaves))
            {
                File.Delete(CaminhoHexChaves);
            }
        }
    }

    private static void RodarComandoNrfjprog(string argumentos)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "nrfjprog",
            Arguments = argumentos,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var processo = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(processo, "Não foi possível iniciar o nrfjprog.");

        processo.WaitForExit();

        string output = processo.StandardOutput.ReadToEnd();
        string erro = processo.StandardError.ReadToEnd();

        if (processo.ExitCode != 0)
        {
            throw new Exception($"Erro do J-Link (Code {processo.ExitCode}): {erro}\n{output}");
        }
    }

    private static byte[] MontarPayloadLorawan(string dev, string app, string key)
    {
        string concatenado = dev + app + key;

        if (concatenado.Length % 2 != 0)
            throw new ArgumentException("As chaves LoRaWAN possuem formato inválido.");

        byte[] bytes = new byte[concatenado.Length / 2];
        for (int i = 0; i < concatenado.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(concatenado.Substring(i, 2), 16);
        }
        return bytes;
    }

    // =========================================================================
    // O GERADOR DE INTEL HEX (Substitui bibliotecas externas)
    // =========================================================================
    private static async Task GerarArquivoIntelHexAsync(byte[] dados, uint enderecoBase, string caminhoArquivo)
    {
        using var writer = new StreamWriter(caminhoArquivo, false, Encoding.ASCII);

        // 1. Registro de Endereço Estendido (Type 04) - Define os 16 bits mais altos do endereço
        ushort upperAddress = (ushort)(enderecoBase >> 16);
        byte upperHigh = (byte)(upperAddress >> 8);
        byte upperLow = (byte)(upperAddress & 0xFF);
        await EscreverLinhaHexAsync(writer, 0x0000, 0x04, [upperHigh, upperLow]);

        // 2. Registros de Dados (Type 00) - Escreve os dados em blocos de 16 bytes
        ushort lowerAddress = (ushort)(enderecoBase & 0xFFFF);
        for (int i = 0; i < dados.Length; i += 16)
        {
            int bytesParaEscrever = Math.Min(16, dados.Length - i);
            byte[] pedaco = new byte[bytesParaEscrever];
            Array.Copy(dados, i, pedaco, 0, bytesParaEscrever);

            await EscreverLinhaHexAsync(writer, (ushort)(lowerAddress + i), 0x00, pedaco);
        }

        // 3. Registro de Fim de Arquivo (Type 01)
        await EscreverLinhaHexAsync(writer, 0x0000, 0x01, []);
    }

    private static async Task EscreverLinhaHexAsync(StreamWriter writer, ushort endereco, byte tipoRegistro, byte[] dados)
    {
        byte contagem = (byte)dados.Length;
        byte enderecoAlto = (byte)(endereco >> 8);
        byte enderecoBaixo = (byte)(endereco & 0xFF);

        int soma = contagem + enderecoAlto + enderecoBaixo + tipoRegistro;
        foreach (var b in dados) soma += b;

        // Padrão do Intel HEX: Complemento de 2 do byte menos significativo da soma
        byte checksum = (byte)((~soma + 1) & 0xFF);

        var linha = new StringBuilder();
        linha.Append($":{contagem:X2}{enderecoAlto:X2}{enderecoBaixo:X2}{tipoRegistro:X2}");
        foreach (var b in dados) linha.Append($"{b:X2}");
        linha.Append($"{checksum:X2}");

        await writer.WriteLineAsync(linha.ToString());
    }
}