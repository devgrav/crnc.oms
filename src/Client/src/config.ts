if(!process.env.REACT_APP_SECURITY_API_URL || !process.env.REACT_APP_SALES_API_URL || !process.env.REACT_APP_PRODUCTION_API_URL || !process.env.REACT_APP_NOTIFICATION_PUSH_HUBS_URL)
    throw Error("Not found api urls in configuration of enviroment variabels")

console.log("REACT_APP_SECURITY_API_URL: "+process.env.REACT_APP_SECURITY_API_URL)
console.log("REACT_APP_SALES_API_URL: "+process.env.REACT_APP_SALES_API_URL)
console.log("REACT_APP_PRODUCTION_API_URL: "+process.env.REACT_APP_PRODUCTION_API_URL)
console.log("REACT_APP_NOTIFICATION_PUSH_HUBS_URL: "+process.env.REACT_APP_NOTIFICATION_PUSH_HUBS_URL)
console.log("NODE_ENV: "+process.env.NODE_ENV)

const securityApiUrl: string = process.env.REACT_APP_SECURITY_API_URL;
const salesApiUrl: string = process.env.REACT_APP_SALES_API_URL; 
const productionApiUrl: string = process.env.REACT_APP_PRODUCTION_API_URL; 
const pushHubsUrl: string = process.env.REACT_APP_NOTIFICATION_PUSH_HUBS_URL; 

const APP_CONFIG: ApplicationConfiguration = {
    usersUrl: `${securityApiUrl}/users`,
    rolesUrl: `${securityApiUrl}/roles`,
    accountsUrl: `${securityApiUrl}/accounts`,
    ordersUrl: `${salesApiUrl}/orders`,
    jobsUrl: `${productionApiUrl}/jobs`,
    pushUrl: `${pushHubsUrl}/push`
};

interface ApplicationConfiguration{
    usersUrl: string;
    rolesUrl: string;
    accountsUrl: string;
    ordersUrl: string;
    jobsUrl: string;
    pushUrl: string;
}

export default APP_CONFIG;
