import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Product } from './models/product';
import { Pagination } from './models/pagination';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  title = 'Vinet';
  products: Product[] = [];

  constructor(private http: HttpClient) {
    // This is a comment
    console.log('Hello from the app component');
  }
  ngOnInit(): void {
    this.http
      .get<Pagination<Product>>(
        'https://localhost:5001/api/products?pageSize=50'
      )
      .subscribe({
        next: (response) => (this.products = response.data),
        error: (err) => console.log(err),
        complete: () => {
          console.log('request completed');
          console.log('extra statement');
        },
      });
  }
}
