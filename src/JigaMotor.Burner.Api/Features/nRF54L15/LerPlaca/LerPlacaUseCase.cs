using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace JigaMotor.Features.nRF54L15.LerPlaca;

// DTO de resposta organizado
public record LerPlacaResponse(string FICR, string DevEui, string AppEui, string AppKey);

public partial class LerPlacaUseCase
{
    private const string EnderecoFicr = "0x00FFC000";
    private const string EnderecoLorawan = "0x0017C000";

    private static readonly char[] SeparadoresLinha = ['\r', '\n'];

    [GeneratedRegex(@":\s*(.*?)\s*\|")]
    private static partial Regex RegexMemrd();

    public async Task<LerPlacaResponse> ExecutarAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                // 1. Lê o FICR (Número de série único da Nordic)
                string rawFicr = RodarComandoNrfjprog($"--family NRF54L --memrd {EnderecoFicr} --n 16");
                string ficrHex = LimparSaidaMemrd(rawFicr);

                // 2. Lê o bloco de chaves LoRaWAN (32 bytes totais: 8 + 8 + 16)
                string rawChaves = RodarComandoNrfjprog($"--family NRF54L --memrd {EnderecoLorawan} --n 32");
                string chavesHex = LimparSaidaMemrd(rawChaves);

                // 3. Fatiamento (Substring) da stringzona Hexadecimal
                // Cada byte tem 2 caracteres Hex. 
                // DevEui (8 bytes = 16 chars), AppEui (8 bytes = 16 chars), AppKey (16 bytes = 32 chars)
                return new LerPlacaResponse(
                    FICR: ficrHex,
                    DevEui: chavesHex.Length >= 16 ? chavesHex[..16] : "ERRO",
                    AppEui: chavesHex.Length >= 32 ? chavesHex[16..32] : "ERRO",
                    AppKey: chavesHex.Length >= 64 ? chavesHex[32..64] : "ERRO"
                );
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PROTECTION"))
                    throw new Exception("Chip trancado (APPROT). Execute 'nrfjprog --recover' para permitir a leitura.");

                throw new Exception($"Falha na leitura via J-Link: {ex.Message}");
            }
        });
    }

    private static string LimparSaidaMemrd(string outputBruto)
    {
        var builder = new StringBuilder();
        var linhas = outputBruto.Split(SeparadoresLinha, StringSplitOptions.RemoveEmptyEntries);

        foreach (var linha in linhas)
        {
            var match = RegexMemrd().Match(linha);
            if (match.Success)
            {
                builder.Append(match.Groups[1].Value.Replace(" ", ""));
            }
        }
        return builder.ToString().ToUpper();
    }

    private static string RodarComandoNrfjprog(string argumentos)
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
        ArgumentNullException.ThrowIfNull(processo, "nrfjprog não encontrado no PATH.");

        processo.WaitForExit();

        string output = processo.StandardOutput.ReadToEnd();
        string erro = processo.StandardError.ReadToEnd();

        if (processo.ExitCode != 0)
            throw new Exception($"{erro} {output}");

        return output;
    }
}