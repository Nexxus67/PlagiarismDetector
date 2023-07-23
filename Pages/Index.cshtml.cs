using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    [BindProperty]
    public IFormFile File1 { get; set; }

    [BindProperty]
    public IFormFile File2 { get; set; }

    [BindProperty]
    public int N { get; set; } = 3;

    public double? PlagiarismScore { get; private set; }

private List<string> GetNGrams(string code, int n)
{
    code = code.ToLower(); // Convert to lowercase
    List<string> ngrams = new List<string>();
    for (int i = 0; i <= code.Length - n; i++)
    {
        string ngram = code.Substring(i, n);
        ngrams.Add(ngram);
    }
    return ngrams;
}

    private double CalculatePlagiarismScore(string code1, string code2, int n)
    {
        if (n <= 0 || n > code1.Length || n > code2.Length)
        {
            throw new ArgumentException("The value of n has to be between 1 and the smallest code size");
        }

        List<string> ngramsCode1 = GetNGrams(code1, n);
        List<string> ngramsCode2 = GetNGrams(code2, n);

        int totalNgrams = ngramsCode1.Count;
        int matches = 0;

        foreach (string ngram in ngramsCode1)
        {
            if (ngramsCode2.Contains(ngram))
            {
                matches++;
            }
        }

        double plagiarismPercentage = (double)matches / totalNgrams * 100;
        return plagiarismPercentage;
    }

    private string RemoveCommentsAndWhitespace(string code)
{
    // Remove single-line comments (// ...)
    code = Regex.Replace(code, @"\/\/.*", "");

    // Remove multi-line comments (/* ... */)
    code = Regex.Replace(code, @"/\*(.|\n)*?\*/", "");

    // Remove whitespaces, tabs, and newlines
    code = Regex.Replace(code, @"\s+", "");

    return code;
}

    public void OnPost()
    {
        if (File1 != null && File2 != null && File1.Length > 0 && File2.Length > 0)
        {
            string code1, code2;
            using (var reader1 = new StreamReader(File1.OpenReadStream()))
            {
                code1 = reader1.ReadToEnd();
            }
            using (var reader2 = new StreamReader(File2.OpenReadStream()))
            {
                code2 = reader2.ReadToEnd();
            }

            PlagiarismScore = CalculatePlagiarismScore(code1, code2, N);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Please, select 2 C# files to compare.");
        }
    }

   public IActionResult OnPostDownloadReport()
{
    if (!PlagiarismScore.HasValue)
    {
        return NotFound();
    }

    var report = $"Resultado de la comparación de archivos:\nPorcentaje de similitud: {PlagiarismScore.Value}%";

    // Generate and save the plagiarism report as a file
    string fileName = "plagiarism_report.txt";
    string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
    System.IO.File.WriteAllText(filePath, report);

    // Download the file
    return File(filePath, "text/plain", fileName);
}
}
