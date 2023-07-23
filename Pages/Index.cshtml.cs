using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    // Método para dividir el código en n-gramas
    private List<string> GetNGrams(string code, int n)
    {
        List<string> ngrams = new List<string>();
        for (int i = 0; i <= code.Length - n; i++)
        {
            string ngram = code.Substring(i, n);
            ngrams.Add(ngram);
        }
        return ngrams;
    }

    // Método para calcular la similitud entre dos códigos usando n-gramas completos
    private double CalculatePlagiarismScore(string code1, string code2, int n)
    {
        if (n <= 0 || n > code1.Length || n > code2.Length)
        {
            throw new ArgumentException("El valor de n debe estar entre 1 y el tamaño del código más pequeño.");
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

    public void OnPost()
    {
        if (File1 != null && File2 != null && File1.Length > 0 && File2.Length > 0)
        {
            // Leer el contenido de los archivos
            string code1, code2;
            using (var reader1 = new StreamReader(File1.OpenReadStream()))
            {
                code1 = reader1.ReadToEnd();
            }
            using (var reader2 = new StreamReader(File2.OpenReadStream()))
            {
                code2 = reader2.ReadToEnd();
            }

            // Calcular el porcentaje de similitud utilizando el algoritmo de detección de plagio
            PlagiarismScore = CalculatePlagiarismScore(code1, code2, N);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Por favor, seleccione dos archivos para comparar.");
        }
    }

    public IActionResult OnPostDownloadReport()
    {
        if (!PlagiarismScore.HasValue)
        {
            return NotFound();
        }

        var report = $"Resultado de la comparación de archivos:\nPorcentaje de similitud: {PlagiarismScore.Value}%";

        var byteArray = Encoding.UTF8.GetBytes(report);
        var stream = new MemoryStream(byteArray);

        return File(stream, "text/plain", "Informe.txt");
    }
}

