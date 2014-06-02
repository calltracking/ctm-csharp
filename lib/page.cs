public class Page<T> {
  public T[]    items;
  public int    total_entries;
  public int    total_pages;
  public int    page;
  public string error;

  public Page(T[] items, int page, int total_entries, int total_pages) {
    this.items         = items;
    this.page          = page;
    this.total_entries = total_entries;
    this.total_pages   = total_pages;
  }

  public Page(string error){
    this.error = error;
  }
}
