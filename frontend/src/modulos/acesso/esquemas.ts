import { z } from 'zod'

/**
 * Regras de validação do cliente.
 *
 * Elas espelham as do servidor, que é quem de fato decide (RNF15): aqui a
 * validação é conveniência, para a pessoa descobrir o problema sem esperar a
 * ida e volta. Onde o servidor recusar algo que estes esquemas deixaram passar,
 * a mensagem dele é a que aparece.
 *
 * O comprimento mínimo da senha é o mesmo de `OpcoesDeAcesso.TamanhoMinimoDaSenha`.
 * Se ele mudar lá, muda aqui — é a duplicação que o par cliente/servidor cobra,
 * e está marcada para não passar despercebida.
 */

export const TAMANHO_MINIMO_DA_SENHA = 8

const email = z
  .string()
  .min(1, 'Informe o e-mail.')
  .email('Informe um e-mail válido.')

const senha = z
  .string()
  .min(1, 'Informe a senha.')
  .min(TAMANHO_MINIMO_DA_SENHA, `A senha deve ter pelo menos ${TAMANHO_MINIMO_DA_SENHA} caracteres.`)

export const esquemaDeAcesso = z.object({
  email,
  senha: z.string().min(1, 'Informe a senha.'),
})

export type DadosDeAcesso = z.infer<typeof esquemaDeAcesso>

export const esquemaDeNovaConta = z
  .object({
    nomeCompleto: z
      .string()
      .min(1, 'Informe o nome completo.')
      .max(150, 'O nome completo deve ter no máximo 150 caracteres.'),
    email,
    confirmacaoDeEmail: z.string().min(1, 'Confirme o e-mail.'),
    telefone: z.string().max(20, 'O telefone deve ter no máximo 20 caracteres.').optional(),
    senha,
    confirmacaoDeSenha: z.string().min(1, 'Confirme a senha.'),
    tipo: z.enum(['Apoiador', 'Gestor']),
  })
  // A comparação fica no esquema, e não no componente, para que a mensagem
  // apareça presa ao campo certo em vez de solta no topo do formulário.
  .refine((dados) => dados.email === dados.confirmacaoDeEmail, {
    path: ['confirmacaoDeEmail'],
    message: 'Os e-mails não coincidem.',
  })
  .refine((dados) => dados.senha === dados.confirmacaoDeSenha, {
    path: ['confirmacaoDeSenha'],
    message: 'A senha e a confirmação não coincidem.',
  })

export type DadosDeNovaConta = z.infer<typeof esquemaDeNovaConta>

/** RF09, primeiro passo: só o endereço para onde o link vai. */
export const esquemaDeRecuperacao = z.object({ email })

export type DadosDeRecuperacao = z.infer<typeof esquemaDeRecuperacao>

/** RF09, segundo passo: a senha nova. O token vem da URL, não do formulário. */
export const esquemaDeNovaSenha = z
  .object({
    senha,
    confirmacaoDeSenha: z.string().min(1, 'Confirme a senha.'),
  })
  .refine((dados) => dados.senha === dados.confirmacaoDeSenha, {
    path: ['confirmacaoDeSenha'],
    message: 'A senha e a confirmação não coincidem.',
  })

export type DadosDeNovaSenha = z.infer<typeof esquemaDeNovaSenha>
