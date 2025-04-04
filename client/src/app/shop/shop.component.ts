import { Component, OnInit } from '@angular/core';
import { Product } from '../shared/models/product';
import { ShopService } from './shop.service';
@Component({
  selector: 'app-shop',
  templateUrl: './shop.component.html',
  styleUrls: ['./shop.component.scss'],
})
export class ShopComponent implements OnInit {
  products: Product[] = [];
  brands: string[] = [];
  types: string[] = [];
  brandIdSelected: string = '';
  typeIdSelected: string = '';
  constructor(private shopService: ShopService) {}
  ngOnInit(): void {
    this.initializeShop();
  }
  initializeShop() {
    this.getBrands();
    this.getTypes();
    this.getProducts();
  }
  getProducts() {
    this.shopService.getProducts().subscribe({
      next: (response) => (this.products = response.data),
      error: (err) => console.log(err),
    });
  }
  getBrands() {
    this.shopService.getBrands().subscribe({
      next: (response) => (this.brands = response),
      error: (err) => console.log(err),
    });
  }
  getTypes() {
    this.shopService.getTypes().subscribe({
      next: (response) => (this.types = response),
      error: (err) => console.log(err),
    });
  }
}
