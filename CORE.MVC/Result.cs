using System;
using System.Collections.Generic;

namespace CORE.MVC
{
    /// <summary>
    /// Mostra resultado de uma ação
    /// </summary>
    public sealed class Result
    {
        private bool success_ = true;
        private List<string> messageList_ = new List<string>();
        private Exception ex;

        /// <summary>
        /// Resultado
        /// </summary>
        public bool Success { get { return success_; } }
        /// <summary>
        /// Lista de mensagens
        /// </summary>
        public string[] MessageList { get { return messageList_.ToArray(); } }
        public string Message { get { return messageList_.Count > 0 ? string.Join("\n", messageList_) : ""; } }
        /// <summary>
        /// Devolve o Exception
        /// </summary>
        public Exception Exception { get { return ex; } }

        /// <summary>
        /// Adiciona mensagem marcada como sucesso
        /// </summary>
        /// <param name="sms">Mensagens</param>
        public void AddMessage(params string[] sms)
        {
            messageList_.AddRange(sms);
            success_ = true;
        }
        /// <summary>
        /// Adiciona mensagem marcada como erro
        /// </summary>
        /// <param name="sms">Mensagens</param>
        public void AddError(params string[] sms)
        {
            messageList_.AddRange(sms);
            success_ = false;
        }
        /// <summary>
        /// Adiciona mensagem marcada como erro
        /// </summary>
        /// <param name="sms">Mensagens</param>
        public void AddError(Exception ex)
        {
            this.ex = ex;
            AddError(ex.Message);
        }

    }

}
