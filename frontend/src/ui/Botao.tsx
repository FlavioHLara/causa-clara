import type { ButtonHTMLAttributes, ReactNode } from 'react'
import './ui.css'

type Aparencia = 'primario' | 'contorno' | 'perigo'

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  aparencia?: Aparencia
  larguraCheia?: boolean
  carregando?: boolean
  children: ReactNode
}

/**
 * Botão do catálogo de componentes.
 *
 * Enquanto `carregando`, fica desabilitado e anuncia o estado por
 * `aria-busy` — quem usa leitor de tela precisa saber que a ação está em
 * curso, não só quem vê o rótulo mudar.
 */
export function Botao({
  aparencia = 'primario',
  larguraCheia = false,
  carregando = false,
  disabled,
  children,
  ...resto
}: Props) {
  const classes = [
    'botao',
    `botao--${aparencia}`,
    larguraCheia ? 'botao--largura-cheia' : '',
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <button
      className={classes}
      disabled={disabled || carregando}
      aria-busy={carregando || undefined}
      {...resto}
    >
      {carregando ? 'Aguarde…' : children}
    </button>
  )
}
