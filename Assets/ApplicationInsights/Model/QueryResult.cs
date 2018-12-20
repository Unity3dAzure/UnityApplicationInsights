using System;

namespace UnityApplicationInsights
{
  [Serializable]
  public class QueryResult<T>
  {
    public Tables<T>[] tables;
  }

  [Serializable]
  public class Tables<T>
  {
    public string name;
    public Columns[] columns;
    public T[] rows;
  }

  [Serializable]
  public class Columns
  {
    public string name;
    public string type;
  }
}