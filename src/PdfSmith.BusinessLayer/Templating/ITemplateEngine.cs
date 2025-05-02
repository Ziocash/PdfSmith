using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSmith.BusinessLayer.Templating;

public interface ITemplateEngine
{
    Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default);
}
