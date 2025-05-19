using PdfSmith.Shared.Enums;

namespace PdfSmith.Shared.Models;

public record class PdfOptions(string PageSize = "A4", PdfOrientation Orientation = PdfOrientation.Portrait, PdfMargin? Margin = null);
