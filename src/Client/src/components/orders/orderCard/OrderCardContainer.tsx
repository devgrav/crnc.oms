import * as React from 'react';
import {Provider, observer} from "mobx-react";
import OrdersRootStore from './OrderCardRootStore';
import { Guid } from 'guid-typescript';
import OrderCardRootStore from './OrderCardRootStore';
import NewOrderCard from './NewOrderCard';
import { Redirect } from 'react-router-dom';

@observer
export default class OrderCardContainer extends React.Component<any> {

  private readonly store: OrderCardRootStore = new OrderCardRootStore();

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
    this.store.setEdtitedOrderId(orderId);
  }

  public render() {
    const {store} = this;

    return (      
      <Provider rootStore={this.store}>        
          {store.editedOrderId === Guid.EMPTY 
          ? <NewOrderCard/>
          : <React.Fragment/>}        
      </Provider>          
    );
  }
}
