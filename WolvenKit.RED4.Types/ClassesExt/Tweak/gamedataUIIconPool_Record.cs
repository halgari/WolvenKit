
namespace WolvenKit.RED4.Types
{
	public partial class gamedataUIIconPool_Record
	{
		[RED("icons")]
		[REDProperty(IsIgnored = true)]
		public CArray<TweakDBID> Icons
		{
			get => GetPropertyValue<CArray<TweakDBID>>();
			set => SetPropertyValue<CArray<TweakDBID>>(value);
		}
	}
}
