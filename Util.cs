public class Node
{
    public string key { get; }
    public int leftId { get; }
    public int rightId { get; }
    public int cost { get; }

    public Node(string setKey, int setLeftId, int setRightId, int setCost)
    {
        key = setKey;
        rightId = setRightId;
        leftId = setLeftId;
        cost = setCost;
    }
}

public class Edge
{
    public Node fromNode { get; }
    public Node toNode { get; }
    public int cost { get; }

    public Edge(Node setFromNode, Node setToNode, int setCost)
    {
        fromNode = setFromNode;
        toNode = setToNode;
        cost = setCost;
    }
}

public class Lattice
{
    public List<Node> nodes { get; } = new();
    public List<Edge> edges { get; } = new();

    public Dictionary<Node, List<Edge>> outEdges { get; } = new();

    public Dictionary<Node, List<Edge>> inEdges { get; } = new();

    public void AddEdge(Node from, Node to, int cost)
    {
        var edge = new Edge(from, to, cost);
        edges.Add(edge);

        if (!outEdges.ContainsKey(from))
            outEdges[from] = new List<Edge>();
        outEdges[from].Add(edge);
        if(!inEdges.ContainsKey(to))
            inEdges[to] = new List<Edge>();
        inEdges[to].Add(edge);

        if (!nodes.Contains(from)) nodes.Add(from);
        if (!nodes.Contains(to)) nodes.Add(to);
    }
}
