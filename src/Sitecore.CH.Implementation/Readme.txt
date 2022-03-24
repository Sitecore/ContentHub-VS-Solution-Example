Implementation main library - This is where should store services and other helpers which are specific to the project and can be used on commands and Az functions
Any created services specific for the implementation should be registed on the ServicesRegistrationCollectionExtensions class of the project
This project is executable, which means it makes use of the following config files:
	appsettings.json
	local.settings.json
	user secrets

This project can access services from the following projects:
	Sitecore.CH.Base
