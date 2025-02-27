import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestService {
  private baseUrl = '/api/test-data';

  constructor(private http: HttpClient) {}

  createTestEOI(): Observable<any> {
    return this.http.post(`${this.baseUrl}/eoi`, {});
  }

  createTestFP(): Observable<any> {
    return this.http.post(`${this.baseUrl}/fp`, {});
  }
}
