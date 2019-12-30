import * as React from 'react';
import OrdersGrid from './ordersGrid/OrdersGrid';
import {Provider} from "mobx-react";
import OrdersGridRootStore from './OrdersRootStore';
import { Guid } from 'guid-typescript';

export default class OrdersGridContainer extends React.Component<any> {

  private readonly rootStore: OrdersGridRootStore = new OrdersGridRootStore();

  private getOrderIdByRouteId(idString: string): string{
      
    if(idString){
      if (idString === "new"){
        return Guid.EMPTY
      }

      if (Guid.isGuid(idString)){
        const id = Guid.parse(idString);
        return id.toString();
      }
    }

    return Guid.EMPTY;
  }

  componentDidMount(){
    let orderId = this.getOrderIdByRouteId(this.props.match.params.id)
    this.rootStore.setEdtitedOrderId(orderId);
  }

  public render() {
    return (      
      <Provider rootStore={this.rootStore}>
        <OrdersGrid/>
      </Provider>          
    );
  }
}
