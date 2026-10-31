namespace CausaClara.Aplicacao.Comum;

/// <summary>
/// Envio de e-mail transacional (RF09, RF24).
/// </summary>
/// <remarks>
/// <para>
/// A Aplicação declara o contrato; quem fala SMTP é a Infraestrutura. Trocar de
/// provedor não toca caso de uso nenhum.
/// </para>
/// <para>
/// <b>Política de falha, que o chamador precisa decidir.</b> O envio acontece
/// <em>depois</em> da gravação, nunca dentro da transação: servidor de e-mail
/// fora do ar não pode desfazer a aprovação de um vínculo que já foi decidida.
/// Este método lança <see cref="FalhaAoEnviarEmail"/> quando o transporte falha,
/// e cabe ao caso de uso escolher entre propagar ou apenas registrar:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <b>RF24</b>, aviso de decisão sobre vínculo: registrar e seguir. A decisão já
/// vale; o aviso é cortesia e pode ser reenviado.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>RF09</b>, redefinição de senha: registrar e responder sempre a mesma coisa
/// ao usuário, tenha o endereço existido ou não. Dizer "falhou o envio" revelaria
/// quais e-mails estão cadastrados.
/// </description>
/// </item>
/// </list>
/// </remarks>
public interface IServicoDeEmail
{
    Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancelamento = default);
}

/// <summary>Falha de transporte no envio. A causa real fica no log, nunca na resposta ao usuário (RNF05, RNF29).</summary>
public sealed class FalhaAoEnviarEmail(string mensagem, Exception? causa = null)
    : Exception(mensagem, causa);
