import * as React from "react";
import { Route, Switch, RouteProps, Redirect } from "react-router";
import EstimatesGrid from "./components/estimates/EstimatesGrid";
import JobsGrid from "./components/jobs/JobsGrid";
import NotFound from "./components/notFound/NotFound";;
import UserCards from "./components/users/UserCards";
import EstimateEdit from "./components/estimates/EstimateEdit";
import Login from "./components/auth/Login";
import CurrentUserContext from "./auth/CurrentUserContext";
import Layout from "./components/layout/Layout";

const Routes: React.StatelessComponent = () => {
    return (
        <Switch>
            <PrivateRoute exact path="/" component={UserCards}/>
            <PrivateRoute path="/users/:id" component={UserCards}/>
            <PrivateRoute path="/users" component={UserCards}/>
            <PrivateRoute path="/estimates/:id" component={EstimateEdit}/>
            <PrivateRoute path="/estimates" component={EstimatesGrid}/>
            <PrivateRoute path="/jobs" component={JobsGrid}/>            
            <Route path="/login" component={Login}/>
            <Route component={NotFound}/>
        </Switch>
    );
};

const PrivateRoute: React.SFC<any> = ({
    component: Component,
    ...rest
  }: {
    component: React.ComponentType<RouteProps>;
  }) => (
    <Route
      {...rest}
      render={props =>
        CurrentUserContext.isAuthentificated 
          ? 
            <Layout>
                <Component {...props} /> 
            </Layout>
            
          : <Redirect to={{
                pathname: "/login",
                state: { from: props.location }
            }}/>
      }
    />
  );

export default Routes;
