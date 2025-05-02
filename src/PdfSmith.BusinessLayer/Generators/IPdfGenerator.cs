namespace PdfSmith.BusinessLayer.Generators;

public interface IPdfGenerator
{
    Task<Stream> CreateAsync(string content, CancellationToken cancellationToken = default);
}
