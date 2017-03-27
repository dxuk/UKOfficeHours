// Settings for app to be loaded into the global scope
// PD Configuration
// We could make this server agnostic by declaring these server-side but this is a js spa so we will simply switch the config file out at deploy time

var instance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

// Figure out what the current URI is and where to get our config from.
var serverprefixaddress = window.location.href.split("/")[2].split(".")[0];
var prefix = serverprefixaddress.split("officehours")[0]
var resource = 'https://' + prefix + 'ukohfn.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource + "/";

// Pick up server specific settings declared in config.js and make them usable in this script
var clientid = '';
var tenantid = ''; // Azure AD Tenant