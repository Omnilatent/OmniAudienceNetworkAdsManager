"Error CS0117: 'AudienceNetworkAds' does not contain a definition for 'IsInitialized'":
AudienceNetwork has functions that are internal and so, can only be accessed from Script inside the same Assembly.
To fix this:
- From the menu, run "Tools/Omnilatent/Ads Manager/Import AudienceNetwork Assembly Fix"
OR
- Add an Assembly Definition Reference to AudienceNetwork SDK's folder that refer to OmniAdsManager's Assembly Definition