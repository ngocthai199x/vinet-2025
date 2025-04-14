import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Address, User } from '../../shared/models/user';
import { map, tap } from 'rxjs';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  private signalrService = inject(SignalrService);
  currentUser = signal<User | null>(null);
  login(values: any) {
    let params = new HttpParams();
    params = params.append('useCookies', true);
    return this.http
      .post<User>(this.baseUrl + 'login', values, {
        params,
      })
      .pipe(tap(() => this.signalrService.createHubConnection()));
  }
  register(values: any) {
    return this.http.post(this.baseUrl + 'account/register', values);
  }
  getUserInfor() {
    return this.http.get<User>(this.baseUrl + 'account/user-info').pipe(
      map((user) => {
        this.currentUser.set(user);
        return user;
      })
    );
  }
  logout() {
    return this.http
      .post(this.baseUrl + 'account/logout', {}, { withCredentials: true })
      .pipe(tap(() => this.signalrService.stopHubConnection()));
  }
  updateAddress(add: Address) {
    return this.http.post(this.baseUrl + 'account/address', add).pipe(
      //use tap because we dont need anything return here
      //use map if me want to do something be still reture the object
      tap(() => {
        this.currentUser.update((user) => {
          if (user) user.address = add;
          return user;
        });
      })
    );
  }
  getAuthState() {
    return this.http.get<{ isAuthenticated: boolean }>(
      this.baseUrl + 'account/auth-status'
    );
  }
}
