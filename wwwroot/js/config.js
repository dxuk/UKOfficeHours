// Settings for app to be loaded into the global scope
// CI / Local Configuration
// We could make this server agnostic by declaring these server-side but this is a js spa so we will simply switch the config file out at deploy time

var prefix = 'dummyci'; // Dummy CI Deployment prefix 
var clientid = '643c52a1-b206-4f15-b0b8-047e8b6bbe9c'; // Dummy CI Deployment Azure AD App 

var authinstance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint

var tenantid = 'microsoft.com'; // Azure AD Tenant
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

// Pick up server specific settings declared in config.js and make them usable in this script
var resource = 'https://' + prefix + 'ukohfn.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource + "/";