import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-unauthorized',
    templateUrl: './unauthorized.component.html',
    styleUrls: ['./unauthorized.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class UnauthorizedComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
