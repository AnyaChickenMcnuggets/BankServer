using Simplex;
using System.Diagnostics;
using System.IO;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.Run(async (context) =>
{   
    string path = @"C:\Users\Ilian\Desktop\simplex.lp";
    StringBuilder subjectTo = new();
    string formatBuilder = "Minimize\n obj:{0}\nSubject To\n{1}End";
    StringBuilder obj = new();
    double[,] table;
    var response = context.Response;
    var request = context.Request;
    if (request.Path == "/api/matrix")
    {
        var message = "AAAA";   // содержание сообщения по умолчанию
        Console.WriteLine(message);
        string resultMessage = "";
        try
        {            
            // пытаемся получить данные json
            var data = await request.ReadFromJsonAsync<ClientData>();
            if (data != null){
                // получаем размерность матрицы
                int n = int.Parse(data.N);
                int m = int.Parse(data.M);
                // выделяем память на матрицу
                table = new double[n, m];
                // делим полученную строку на строки
                string[] items = data.Matrix.Split(';');
                // бежим по каждому элементу строки
                foreach (string item in items)
                {   
                    Console.WriteLine(item);
                    n = int.Parse(item.Split('|')[0]);
                    m = int.Parse(item.Split('|')[1]);
                    double value = double.Parse(item.Split('|')[2].Replace('.',','));
                    table[n, m] = value;
                }
                double[] result = new double[m];
                double[,] table_result;
                Simplex.Simplex S = new(table);
                table_result = S.Calculate(result);

                // берем все элементы целевой кроме 0, там 0
                for (int i = 1; i<=m; i++){
                    if (table[n,i]<0){
                        obj.Append(" - ");
                        obj.Append(-1* table[n,i]);
                    } else{
                        obj.Append(" + ");
                        obj.Append(table[n,i]);
                    }
                    
                    obj.Append(" x");
                    obj.Append(i);
                }
                bool firstIsWritten = false;
                string objWrite = obj.ToString();
                for (int i = 0; i<=n-1;i++){
                    for (int j = 1; j<=m; j++){
                        if(j==1){
                            subjectTo.Append($" c{i+1}:");
                        }
                        if(table[i,j]!=0){
                            if (table[i,j]<0){
                                subjectTo.Append(" - ");
                                if (table[i,j]!=-1) subjectTo.Append(-1* table[i,j]);
                            } else{
                                if(firstIsWritten){
                                    subjectTo.Append(" + ");
                                }                                
                                if (table[i,j]!=1) subjectTo.Append(table[i,j]);
                            }
                            subjectTo.Append(" x");
                            subjectTo.Append(j);
                            firstIsWritten = true;
                        }        

                        if(j==m){
                            subjectTo.Append(" <= ");
                            subjectTo.Append(table[i,0]);
                            subjectTo.Append("\n");
                            firstIsWritten = false;
                        }
                    }
                }
                for (int i = 0; i < table.GetLength(0); i++)
                {
                    for (int j = 0; j < table.GetLength(1); j++)
                        Console.Write(table[i, j] + " ");
                    Console.WriteLine(";");
                }
                
                Guid myuuid = Guid.NewGuid();
                string myuuidAsString = myuuid.ToString();
                string tableString = string.Format(formatBuilder, objWrite, subjectTo);
                File.WriteAllText(path + myuuidAsString + ".lp", tableString);
                

                Console.WriteLine();
                Console.WriteLine("Решение:");


                Console.WriteLine(result);
                for(int i = 0; i< result.Length; i++){
                    Console.WriteLine($"X[{i+1}] = " + result[i]);
                    resultMessage += result[i];
                    resultMessage += "|";
                }                
            }                 
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

public record ClientData(string Matrix, string N, string M);


