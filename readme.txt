monogame 공부중
만드는 중이라 정리 안되어있음



* 프로젝트 여는 방법

project1 solution 을 열면, 다음 2개의 project 가 있음

* MGAEditor project
* Project1 project

최초 1회 실행하면 bin directory 에 config.toml 파일이 생긴다.
여기에서 asset file 들의 path 를 설정해 줘야 한다. <- 나중에 자동화 해야함

* doc

doc/html/index.html <- 을 열면 doxygen 으로 생성한 문서 있음

* 새 프로젝트 만들기
monogame - desktop GL template 을 이용해서 새 프로젝트 만들기
원하는 곳에 asset folder 만들기 
MGA project 에 참조와 dependency 설정
한번 실행후 생성된 config.toml 에 위에서 만든 asset folder 설정