import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Pagination } from '../shared/models/pagination';
import { Product } from '../shared/models/product';

@Injectable({
  providedIn: 'root',
})
export class ShopService {
  baseUrl = 'https://localhost:5001/api/';
  constructor(private http: HttpClient) {}
  getProducts(brandId?: string, typeId?: string) {
    let params = new HttpParams();
    if (brandId) {
      params = params.append('brand', brandId);
    }
    if (typeId) {
      params = params.append('type', typeId);
    }
    return this.http.get<Pagination<Product>>(this.baseUrl + 'products', {
      params,
    });
  }
  getBrands() {
    return this.http.get<string[]>(this.baseUrl + 'products/brands');
  }
  getTypes() {
    return this.http.get<string[]>(this.baseUrl + 'products/types');
  }
}
