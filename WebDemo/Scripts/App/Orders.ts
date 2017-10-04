﻿import { Component, OnInit, Injectable } from '@angular/core';
import * as utils from './lib/utils';
import * as models from './models';

@Component({
    template: `
<h1>Orders</h1>
<data-grid [settings]="orders"></data-grid>
    <data-area [settings]="orders" [columns]="2" ></data-area>
    <data-area [settings]="area1" ></data-area>
`
})

@Injectable()
export class Orders {

    customers = new utils.Lookup<models.customer, string>(apiUrl + 'customers');
    orders = new utils.DataSettings<models.order>({
        restUrl: apiUrl + "orders",
        allowUpdate: true,
        hideDataArea: true
    });
    area1 = this.orders.addArea(
        {
            labelWidth:2,
            columnSettings:
            [{ key: "shipName", caption: "ShipName" },
            { key: "shipAddress", caption: "ShipAddress" },
            { key: "shipCity", caption: "ShipCity" },
            { key: "shipRegion", caption: "Ship Region" },
            { key: "shipPostalCode", caption: "ShipPostalCode" },
            { key: "shipCountry", caption: "ShipCountry" }]
        });




}
const apiUrl = '/dataApi/';