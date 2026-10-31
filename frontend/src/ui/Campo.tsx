import { forwardRef, useId } from 'react'
import type { InputHTMLAttributes } from 'react'
import './ui.css'

type Props = InputHTMLAttributes<HTMLInputElement> & {
  rotulo: string
  erro?: string
  dica?: string
}

/**
 * Campo de texto com rótulo, dica e mensagem de erro.
 *
 * Três cuidados de acessibilidade que o protótipo exige (RNF07, RNF08, RNF05):
 * o rótulo é um `<label>` ligado ao campo, não um texto solto; o erro é
 * apontado por `aria-describedby` e marcado com `aria-invalid`, de modo que o
 * leitor de tela o anuncie ao chegar no campo; e a mensagem vem com símbolo e
 * texto, nunca só com a borda vermelha.
 */
export const Campo = forwardRef<HTMLInputElement, Props>(function Campo(
  { rotulo, erro, dica, id, ...resto },
  referencia,
) {
  const gerado = useId()
  const idDoCampo = id ?? gerado
  const idDoErro = `${idDoCampo}-erro`
  const idDaDica = `${idDoCampo}-dica`

  const descritores = [erro ? idDoErro : null, dica ? idDaDica : null]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={`campo${erro ? ' campo--com-erro' : ''}`}>
      <label className="campo__rotulo" htmlFor={idDoCampo}>
        {rotulo}
      </label>

      <input
        ref={referencia}
        id={idDoCampo}
        className="campo__entrada"
        aria-invalid={erro ? true : undefined}
        aria-describedby={descritores || undefined}
        {...resto}
      />

      {dica && (
        <span id={idDaDica} className="texto-apoio">
          {dica}
        </span>
      )}

      {erro && (
        <span id={idDoErro} className="campo__erro" role="alert">
          <span aria-hidden="true">⚠</span>
          {erro}
        </span>
      )}
    </div>
  )
})
