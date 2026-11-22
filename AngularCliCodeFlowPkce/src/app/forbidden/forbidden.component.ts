import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-forbidden',
    templateUrl: './forbidden.component.html',
    styleUrls: ['./forbidden.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ForbiddenComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
