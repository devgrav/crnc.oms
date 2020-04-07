import * as React from 'react';
import OrdersGrid from './JobsGrid';
import {Provider} from "mobx-react";
import { Guid } from 'guid-typescript';
import JobsGrid from './JobsGrid';
import JobsGridRootStore from './JobsGridRootStore';
import JobsRootStores from '../JobsRootStores';

export default class JobsGridContainer extends React.Component<any> {

  private readonly stores: JobsRootStores = {
    jobsGridRootStore : new JobsGridRootStore()
  }

  public render() {
    return (      
      <Provider {...this.stores}>
        <JobsGrid/>
      </Provider>          
    );
  }
}
