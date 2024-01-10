using System.Text.RegularExpressions;
/// <summary>
/// creez tabelul de simboluri si le daug valoarea dupa ce le citesc
/// urmeaza inca un parsing 
/// Dictionary (string, int)
/// vezi de la pagina 110 in suportul de curs
/// </summary>
class Program
{
    static Dictionary<string, int> symbols = new Dictionary<string, int>();
    static List<(string line, int number)> linesOfCode = new();
    static int locationStart = 2048;
    static void Main()
    {
        string path = "input2.txt";
        StreamReader reader = new(path);
        string line;
        //first parse : deletes comments
        while ((line = reader.ReadLine()) != null)
        {
            line = Regex.Replace(line, @"!.*", ""); //deletes comments
            if(line != "")
            {
                linesOfCode.Add((line, locationStart));
                if (!line.Contains('.'))
                {
                    if (line.Contains(':'))
                    {
                        string[] tokens = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        symbols.Add(tokens[0], locationStart);
                    }
                    locationStart += 4;
                }
                else
                { 
                    Regex regex = new Regex(@",*\s+"); //split on , and white spaces

                    string[] parts = regex.Split(line).Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();
                    if (parts.Contains(".end"))
                        break;
                    if(parts.Contains(".equ"))
                        symbols.Add(parts[0], int.Parse(parts[2]));
                    if (parts.Contains(".org"))
                    {
                        int nr;
                        if (int.TryParse(parts[1], out nr))
                            locationStart = nr;
                        else locationStart = symbols[parts[1]];
                    }
                    
                }
            }
        }


        foreach(string token in symbols.Keys)
        {
            Console.WriteLine(token + " " + symbols[token]);
        }

        foreach (var code in linesOfCode)
        {
            if(!code.line.Contains("."))
            {
                line = code.line;
                if(code.line.Contains(":")) line = Regex.Replace(line, @"\w+:\s*", "");
                Regex regex = new(@",*\s+");
                string[] parts = regex.Split(line).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                foreach(var item in parts)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
                string binaryCode = ConvertToBinary(parts, code.number);
                    
                
            }
        }


    }

    private static string ConvertToBinary(string[] parts, int lineNr)
    {
        if(parts.Length == 1)
        {
            if (symbols.ContainsKey(parts[0]))
                return ConvertStringToBinary(symbols[parts[0]].ToString(), 32);
        }

        string op = GetOp(parts[0]);
        string binary = op;
        switch(op)
        {
            case "00":
                {
                    if (parts[0] == "sethi")
                    {
                        string secondArg = GetRegisterNr(parts[2]);
                        binary += ConvertStringToBinary(secondArg, 5);
                        binary += ConvertStringToBinary(parts[1], 22);
                    }
                    else
                    {
                        string firstPart = "0";
                        firstPart += GetCond(parts[0]);
                        firstPart += GetOp2(parts[0]);
                        binary += firstPart;
                        int nr;
                        if (int.TryParse(parts[1], out nr))
                        {
                            int numberToAdd = (nr - lineNr) / 4;
                            binary += ConvertStringToBinary(numberToAdd.ToString(), 22);
                        }
                        else
                        {
                            if (symbols.ContainsKey(parts[1]))
                            {
                                int numberToAdd = (symbols[parts[1]] - lineNr) / 4;
                                binary += ConvertStringToBinary(numberToAdd.ToString(), 22);
                            }
                            else
                            {
                                throw new Exception("Error in code"); // maybe it's undefined
                            }
                        }
                    }
                }break;
            case "01":
                {
                    int nr;
                    if (int.TryParse(parts[1], out nr))
                        binary += ConvertStringToBinary((nr - lineNr).ToString(), 30);
                    else
                    {
                        int number = symbols[parts[1]] - lineNr;
                        binary += ConvertStringToBinary(number.ToString(), 30);
                    }
                }break;
            case "10":
                {
                    string rd, rs1, rs2, simm13, op3;
                    op3 = GetOp3(parts[0]);     //need a for to loop over tokens
                    

                }break;
        }
        return "-1";
    }

    private static string ConvertStringToBinary(string number, int lenght)
    {
        string binary;
        if (number.Contains("0x"))
        {
            int converted = Convert.ToInt32(number);
            binary = Convert.ToString(converted, 2);
        }
        else binary = Convert.ToString(int.Parse(number), 2);

        binary = binary.PadLeft(32, '0');
        if (binary.Length > lenght)
            binary = binary.Remove(0, binary.Length - lenght);

        return binary;
    }
    private static string GetRegisterNr(string v)
    {
        v = Regex.Replace(v, @"\[]/|\%r", "");
        Console.WriteLine(v);
        return v;
    }


    static string GetOp(string text)
    {
        switch (text)
        {
            case "be":
            case "bcs":
            case "bneg":
            case "bvs":
            case "ba":
            case "sethi":
                return "00";
            case "addcc":
            case "andcc":
            case "orcc":
            case "orncc":
            case "srl":
            case "jmpl":
                return "10";
            case "ld":
            case "st":
                return "11";
            case "call":
                return "01";
        }
        return "";
    }

    static string GetOp2(string text)
    {
        switch (text)
        {
            //branch
            case "be":
            case "bcs":
            case "bneg":
            case "bvs":
            case "ba":
                return "010";
            //sethi
            case "sethi":
                return "100";
        }
        return "";
    }

    static string GetOp3(string text)
    {
        switch(text)
        {
            case "ld":
                return "000000";
            case "st":
                return "000100";
            case "addcc":
                return "010000";
            case "andcc":
                return "010001";
            case "orcc":
                return "010010";
            case "orncc":
                return "010110";
            case "srl":
                return "100110";
            case "jmlp":
                return "111000";
        }
        return "";
    }
    static string GetCond(string text)
    {
        switch (text)
        {
            case "be":
                return "0001";
            case "bcs":
                return "0101";
            case "bneg":
                return "0110";
            case "bvs":
                return "0111";
            case "ba":
                return "1000";
        }
        return "";
    }

}