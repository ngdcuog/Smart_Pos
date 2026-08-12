namespace SmartPOS.Services; public interface IAIChatService{Task<string> AskAsync(string question,CancellationToken token=default);}
