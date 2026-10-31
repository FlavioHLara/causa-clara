/**
 * Tipos do módulo de acesso.
 *
 * Escritos à mão por enquanto. A partir do momento em que houver mais
 * endpoints, eles passam a ser gerados do documento OpenAPI por
 * `npm run tipos`, de modo que renomear um campo no servidor quebre a
 * compilação do cliente em vez de quebrar em produção.
 */

export type TipoDeConta = 'Apoiador' | 'Gestor'

export type PerfilDoUsuario = {
  id: number
  nomeCompleto: string
  email: string
  telefone: string | null
  tipo: TipoDeConta
  instituicaoId: number | null
}

export type SessaoAberta = {
  tokenDeAcesso: string
  expiraEm: string
  tokenDeAtualizacao: string
  perfil: PerfilDoUsuario
}

export type NovaConta = {
  nomeCompleto: string
  email: string
  telefone: string | null
  senha: string
  confirmacaoDeSenha: string
  tipo: TipoDeConta
}
