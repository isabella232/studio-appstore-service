namespace AppStoreIntegrationService.Model
{
	public enum ModalType
	{
		WarningMessage,
		ConfirmationMessage,
		SuccessMessage
	}

	public class ModalMessage
	{
		public string PluginName { get; set; }
		public string Message { get; set; }
		public string Title { get; set; }
		public ModalType ModalType { get; set; }	
		public string RequestPage { get; set; }
		public int Id { get; set; }
	}
}
