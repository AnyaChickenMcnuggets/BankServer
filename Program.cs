using Simplex;
using System.Diagnostics;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.Run(async (context) =>
{
    double[,] table;
    var response = context.Response;
    var request = context.Request;
    if (request.Path == "/api/matrix")
    {
        var message = "AAAA";   // содержание сообщения по умолчанию
        Console.WriteLine(message);
        String resultMessage = "";
        try
        {
            
            // пытаемся получить данные json
            var data = await request.ReadFromJsonAsync<ClientData>();
            if (data != null){
                int n = Int32.Parse(data.n);
                int m = Int32.Parse(data.m);
                table = new double[n, m];
                String[] items = data.matrix.Split(';');
                foreach (String item in items)
                {   
                    Console.WriteLine(item);
                    n = Int32.Parse(item.Split('|')[0]);
                    m = Int32.Parse(item.Split('|')[1]);
                    Double value = Double.Parse(item.Split('|')[2].Replace('.',','));
                    table[n, m] = value;
                }
                for (int i = 0; i < table.GetLength(0); i++)
                {
                    for (int j = 0; j < table.GetLength(1); j++)
                        Console.Write(table[i, j] + " ");
                    Console.Write(";");
                }

                // var s = new Simplex.Simplex(
                //     new [] {10.2, 422.3, 6.91, 853},
                //     new [,] {
                //     {0.1, 0.5, 0.333333, 1},
                //     {30, 15, 19, 12},
                //     {1000, 6000, 4100, 9100},
                //     {50, 20, 21, 10},
                //     {4, 6, 19, 30}
                //     },
                //     new double[] {2000, 1000, 1000000, 640, 432}
                // );
                double[] result = new double[m];
                double[,] table_result;
                Simplex.SimplexA S = new Simplex.SimplexA(table);
                table_result = S.Calculate(result);

                Console.WriteLine();
                Console.WriteLine("Решение:");
                Console.WriteLine(result);
                for(int i = 0; i< result.Length; i++){
                    Console.WriteLine($"X[{i+1}] = " + result[i]);
                    resultMessage += result[i];
                    resultMessage += "|";
                }
                //Console.ReadLine();
            } // если данные сконвертированы в Person
                
        }
        catch { }
        // отправляем пользователю данные
        await response.WriteAsJsonAsync(new { text = resultMessage });
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});
app.Run();

public record ClientData(String matrix, String n, String m);
