Base project - This project should used to group services and helpers which are not specific for the implementation, and could be shared with other implementations
Any created services specific should be registed on the ServicesRegistrationCollectionExtensions class of the project
This project is executable, which means it makes use of the following config files:
	appsettings.json
	local.settings.json
	user secrets

This is the project which references the WebSDK, so any upgrade operation, this is the the project where the nuget packages need to be updated