import { createContext, useContext } from 'react'
import type { PerfilDoUsuario } from '../modulos/acesso/tipos'

export type Sessao = {
  perfil: PerfilDoUsuario | null
  conferindo: boolean
  entrar: (email: string, senha: string) => Promise<void>
  sair: () => Promise<void>
}

/**
 * Contexto e gancho da sessão, separados do componente que os provê.
 *
 * A separação não é estética: um arquivo que exporta componente **e** outras
 * coisas quebra o recarregamento rápido do Vite, que só consegue trocar o
 * módulo em memória quando ele exporta apenas componentes.
 */
export const ContextoDaSessao = createContext<Sessao | null>(null)

export function useSessao(): Sessao {
  const contexto = useContext(ContextoDaSessao)
  if (!contexto) throw new Error('useSessao precisa estar dentro de SessaoProvedor.')
  return contexto
}
