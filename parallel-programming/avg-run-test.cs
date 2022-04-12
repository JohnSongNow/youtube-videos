using System;

class AvgRunTest
{

    static int n = 4096;
    static int[,] a = new int[n, n];

    static void Main()
    {
        Random r = new Random();        
        // randomizing our initial problem
        for(int row = 0; col < n; row++){
            for(int col = 0; col < n; col++){
                a[row][col] = r.Next(1, 10);
            }
        }

        // starting out timer
        var watch = System.Diagnostics.Stopwatch.StartNew();
        
        // running avg for each array
        double totalAvg = 0.0;
        for(int row = 0; col < n; row++){
            double localAvg = 0.0;
            for(int col = 0; col < n; col++){
                localAvg += col;
            }
            localAvg = localAvg / n;
            totalAvg += localAvg;
        }

        // getting final results
        watch.Stop();
        double elasped = watch.ElaspsedMilliseconds;
        totalAvg = totalAvg / n;

        // the total_avg is a debug function to check that the randomized values within range of expected
        System.Console.WriteLine("The loop took: " + str(elasped) + " ms and the avg is: " + str(totalAvg));
    }
}
