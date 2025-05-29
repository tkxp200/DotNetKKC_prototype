using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NReco.Text;

namespace DotNetKKC;

public class Conversion
{
    AhoCorasickDoubleArrayTrie<int> doubleArrayTrie;
    List<(int, int, int, string)> dictionary = new List<(int, int, int, string)>();
    int[,] connectionCost;

    public Conversion(string filePath)
    {
        doubleArrayTrie = new AhoCorasickDoubleArrayTrie<int>();
        Init(filePath);
    }

    void Init(string filePath)
    {
        LoadDoubleArray(filePath);
        CreateDictionary();
    }

    void LoadDoubleArray(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        doubleArrayTrie.Load(fileStream);
    }

    void CreateDictionary()
    {
        int index = 0;
        for (int i = 0; i < 10; ++i)
        {
            var filePath = Path.Combine("dic", $"dictionary{i:00}.txt");
            var result = File.ReadAllLines(filePath);
            foreach (var line in result)
            {
                var fields = line.Split('\t');
                dictionary.Add((Convert.ToInt32(fields[1]), Convert.ToInt32(fields[2]), Convert.ToInt32(fields[3]), fields[4]));
                index++;
            }
        }

        var costFilePath = Path.Combine("dic", "connection_single_column.txt");
        var lines = File.ReadAllLines(costFilePath);
        var idLength = Convert.ToInt32(lines[0]);
        connectionCost = new int[idLength, idLength];
        for (int i = 0; i < idLength; ++i)
        {
            for (int j = 0; j < idLength; ++j)
            {
                connectionCost[i, j] = Convert.ToInt32(lines[idLength * i + j + 1]);
            }
        }
    }

    public Dictionary<string, double> GetConversion(string text, int n = 10)
    {
        Console.WriteLine($"Convert: {text}");
        var result = doubleArrayTrie.ParseText(text);

        List<Node>[] parentNodeList = new List<Node>[text.Length + 1]; //Contain BOS
        Node bos = new Node("", 0, 0, 0); //BOS RightID = 0, cost = 0
        Lattice lattice = new Lattice();
        parentNodeList[0] = new List<Node>();
        parentNodeList[0].Add(bos);

        foreach (var hit in result)
        {
            var node = new Node(dictionary[hit.Value].Item4,
                    dictionary[hit.Value].Item1, dictionary[hit.Value].Item2, dictionary[hit.Value].Item3);
            if (parentNodeList[hit.Begin + hit.Length] == null)
                parentNodeList[hit.Begin + hit.Length] = new List<Node>();
            parentNodeList[hit.Begin + hit.Length].Add(node);
            foreach (var parent in parentNodeList[hit.Begin])
            {
                lattice.AddEdge(parent, node, connectionCost[parent.rightId, node.leftId]);
            }
        }
        Node eos = new Node("", 0, 0, 0);
        foreach (var parent in parentNodeList[text.Length])
        {
            lattice.AddEdge(parent, eos, connectionCost[parent.rightId, eos.leftId]);
        }

        var bestCosts = SetBestCosts(lattice, bos);

        return GetNBestConversion(lattice, bestCosts, bos, eos, n);

    }

    Dictionary<Node, double> SetBestCosts(Lattice lattice, Node bos)
    {
        Dictionary<Node, double> bestCosts = new();

        bestCosts[bos] = 0;

        foreach (var edge in lattice.edges)
        {
            var from = edge.fromNode;
            var to = edge.toNode;
            var cost = bestCosts.TryGetValue(from, out var fromCost) ? fromCost + edge.cost + to.cost : double.PositiveInfinity;

            if (!bestCosts.ContainsKey(to) || cost < bestCosts[to])
            {
                bestCosts[to] = cost;
            }
        }

        return bestCosts;
    }

    Dictionary<string, double> GetNBestConversion(Lattice lattice, Dictionary<Node, double> bestCosts, Node bos, Node eos, int n)
    {
        //node, text, BackwardCost, HeuristicCost
        PriorityQueue<(Node, string, double), double> queue = new();
        Dictionary<string, double> results = new();

        if (n <= 0) return results;

        queue.Enqueue((eos, eos.key, 0), eos.cost);

        while (queue.TryDequeue(out (Node, string, double) item, out double cost))
        {
            if (item.Item1 == bos)
            {
                if (!results.ContainsKey(item.Item2))
                {
                    results.Add(item.Item2, cost);
                }
            }
            else
            {
                foreach (var edge in lattice.inEdges[item.Item1])
                {
                    double backwardCost = item.Item1.cost + item.Item3 + edge.cost;
                    double heuristicCost = backwardCost + bestCosts[edge.fromNode];
                    queue.Enqueue((edge.fromNode, item.Item1.key + item.Item2, backwardCost), heuristicCost);
                }
            }
            if (results.Count == n) return results;
        }

        return results;
    }
}

public class DoubleArrayGenerator
{
    List<KeyValuePair<string, int>> dic = new List<KeyValuePair<string, int>>();
    AhoCorasickDoubleArrayTrie<int> doubleArrayTrie;

    public DoubleArrayGenerator(string filePath)
    {
        LoadMozcDictionary();
        BuildDoubleArrayFile(filePath);
    }

    void LoadMozcDictionary()
    {
        int index = 0;
        for(int i = 0; i < 10; ++i)
        {
            var filePath = Path.Combine("dic", $"dictionary{i:00}.txt");
            var result = File.ReadAllLines(filePath);
            foreach(var line in result)
            {
                var fields = line.Split('\t');
                dic.Add(new KeyValuePair<string, int>(fields[0], index));
                index++;
            }
        }
    }

    void BuildDoubleArrayFile(string filePath)
    {
        doubleArrayTrie = new AhoCorasickDoubleArrayTrie<int>(dic);
        using var fileStream = new FileStream(filePath, FileMode.Create);
        doubleArrayTrie.Save(fileStream, true);
    }
}
