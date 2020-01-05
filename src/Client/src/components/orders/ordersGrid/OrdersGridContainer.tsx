import * as React from 'react';
import OrdersGrid from './OrdersGrid';
import {Provider} from "mobx-react";
import OrdersGridRootStore from './OrdersGridRootStore';
import { Guid } from 'guid-typescript';
import OrdersRootStores from '../OrdersRootStores';

export default class OrdersGridContainer extends React.Component<any> {

  private readonly stores: OrdersRootStores = {
    ordersGridRootStore : new OrdersGridRootStore()
  }

  public render() {
    return (      
      <Provider {...this.stores}>
        <OrdersGrid/>
      </Provider>          
    );
  }
}
