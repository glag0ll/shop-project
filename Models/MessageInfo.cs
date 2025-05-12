namespace AvaloniaApplication3.Models
{
    public class MessageInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Info;

        public MessageInfo(string message, MessageType type = MessageType.Info, string title = "")
        {
            Message = message;
            Type = type;
            Title = string.IsNullOrEmpty(title) ? GetDefaultTitle(type) : title;
        }

        private string GetDefaultTitle(MessageType type)
        {
            return type switch
            {
                MessageType.Success => "Успешно!",
                MessageType.Error => "Ошибка!",
                MessageType.Warning => "Внимание!",
                MessageType.Info => "Информация",
                _ => "Сообщение"
            };
        }
    }
} 