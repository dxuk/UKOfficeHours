// Settings for app to be loaded into the global scope
// DEV Configuration
// We could make this server agnostic by declaring these server-side but this is a js spa so we will simply switch the config file out at deploy time

var prefix = 'dev' // Dummy CI Deployment prefix 
var clientid = ''; // Dummy CI Deployment Azure AD App 

var instance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint

var tenantid = 'microsoft.com'; // Azure AD Tenant
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

// Pick up server specific settings declared in config.js and make them usable in this script
var resource = 'https://' + prefix + '-ukofficehours.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource;