import OrderCardRootStore from "./orderCard/OrderCardRootStore";
import OrdersGridRootStore from "./ordersGrid/OrdersGridRootStore";

export default interface OrdersRootStores{
    ordersGridRootStore?: OrdersGridRootStore;
    orderCardRootStore?: OrderCardRootStore;
}