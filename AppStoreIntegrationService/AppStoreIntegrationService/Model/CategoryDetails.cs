namespace AppStoreIntegrationService.Model
{
	public class CategoryDetails
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int? ParentCategoryID { get; set; }
	}
}
