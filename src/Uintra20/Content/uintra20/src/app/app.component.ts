import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, ActivationEnd, Router } from "@angular/router";
import { LoginPage } from "./ui/pages/login/login-page.component";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.less"]
})
export class AppComponent {
  title = "uintra20";

  isLoginPage: boolean = true;
  hasPanels: boolean = false;

  data: any;
  latestActivities: any;
  constructor(private router: Router, private route: ActivatedRoute) {
    this.route.data.subscribe(data => {
      this.data = data;
      this.hasPanels = data && data.panels && data.panels.get();
    });

    router.events.subscribe(val => {
      if (val instanceof ActivationEnd) {
        if (val.snapshot.component) {
          this.isLoginPage = val.snapshot.component === LoginPage;
        }
      }
    });

  }

  ngOnInit(): void {

  }
}
