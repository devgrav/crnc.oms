import * as React from "react";
import { Route, Switch, RouteProps, Redirect } from "react-router";
import OrdersGrid from "./components/orders/ordersGrid/OrdersGrid";
import NotFound from "./components/notFound/NotFound";;
import UserCards from "./components/users/UserCards";
import Login from "./components/auth/Login";
import CurrentUserContext from "./auth/CurrentUserContext";
import Layout from "./components/layout/Layout";
import {Roles, CurrentUserRole} from "./auth/CurrentUserRole";
import Forbidden from "./components/forbidden/Forbidden";
import OrdersGridContainer from "./components/orders/ordersGrid/OrdersGridContainer";
import OrderCardContainer from "./components/orders/orderCard/OrderCardContainer";

const Routes: React.StatelessComponent = () => {
    return (
        <Switch>
            <PrivateRoute roles={[Roles.Admin]} exact path="/" component={UserCards}/>
            <PrivateRoute roles={[Roles.Admin]} path="/users/:id" component={UserCards}/>
            <PrivateRoute roles={[Roles.Admin]} path="/users" component={UserCards}/>            
            <PrivateRoute roles={[Roles.MainManager, Roles.Manager]} path="/orders/:id" component={OrderCardContainer}/>
            <PrivateRoute roles={[Roles.MainManager, Roles.Manager]} path="/orders" component={OrdersGridContainer}/> 
            <Route path="/login" component={Login}/>
            <Route component={NotFound}/>
        </Switch>
    );
};

const PrivateRoute: React.SFC<any> = ({
    component: Component,    
    ...rest
  }: {
    component: React.ComponentType<any>
  }) =>    
    <Route {...rest} render={props => {
        let roles = rest && (rest as any).roles
        return CurrentUserContext.isAuthentificated 
        ? 
          <Layout>
            {
              ((CurrentUserContext.user.role === Roles.Admin) || (roles && roles instanceof Array && roles.some(r => r ===  CurrentUserContext.user.role)))
                ?
                  <Component {...props} />          
                : <Forbidden/>
            }            
          </Layout>
          
        : <Redirect to={{
              pathname: "/login",
              state: { from: props.location }
          }}/>
      } 
    }     
    />;
  
export default Routes;
