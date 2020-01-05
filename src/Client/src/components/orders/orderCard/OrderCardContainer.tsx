import * as React from 'react';
import {Provider, observer} from "mobx-react";
import OrdersRootStore from './OrderCardRootStore';
import { Guid } from 'guid-typescript';
import OrderCardRootStore from './OrderCardRootStore';
import NewOrderCard from './NewOrderCard';
import { Redirect } from 'react-router-dom';
import OrdersRootStores from '../OrdersRootStores';
import EditOrderCard from './EditOrderCard';

@observer
export default class OrderCardContainer extends React.Component<any> {

  private readonly stores: OrdersRootStores
  private readonly rootStore: OrderCardRootStore

  constructor(props: any){
    super(props)

    let orderId = this.getOrderIdByRouteId(this.props.match.params.id)

    this.rootStore  = new OrderCardRootStore(orderId);

    this.stores = {
      orderCardRootStore : this.rootStore
    }
  }

  private getOrderIdByRouteId(idString: string): Guid{
      
    if(idString){
      if (idString === "new"){
        return Guid.parse(Guid.EMPTY)
      }

      if (Guid.isGuid(idString)){
        const id = Guid.parse(idString);
        return id;
      }
    }

    return Guid.parse(Guid.EMPTY);
  }

  public render() {
    const {rootStore} = this;

    return (      
      <Provider {...this.stores}>        
          {rootStore.editedOrderId.isEmpty()
          ? 
            <NewOrderCard/>
          : 
            <EditOrderCard/>
          }
     
      </Provider>          
    );
  }
}
