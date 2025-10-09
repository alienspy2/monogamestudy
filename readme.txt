monogame 공부중
  만드는 중이라 정리 안되어있음.
  일단 넣을 것을 다 넣고, 동작하는 상태로 만든 후 정리하자

  * unity 엔진과 비슷한 interface 를 지향. 익숙한 unity 엔진으로부터 쉽게 전환할 수 있도록 한다
  * 범용엔진을 지향하지 않는다.
    - 지향점 1 코딩 편의성
    - 지향점 2 실행속도
    - 지향점 3 렌더링 퀄리티
  * monogame 은 왼손 좌표계이고, unity 는 오른손 좌표계 이다. 
    따라서 유니티와 좌표계를 똑같이 맞출 수가 없다.
    - MGA 에서는 -Z 가 forward 이고, unity 에서는 +Z 가 forward 이다
    - 그 외에는 동일하다.
  * MGCB 는 shader 외에는 쓰지 말자
    - 쓰기도 번거롭고 지원하지 않는 asset 은 어차피 빌드해주지 않는다. 예) 3D mesh
    - asset build 시 옵션이 부족하다. 예) dds 압축시 quality 설정이 불가
  * 2D만 쓰려다가 3D 로 전환중. 따라서 여러가지를 고쳐야 한다.
  * blender 에서 glb 로 저장하는 것을 표준으로 삼는다. export 시 up = +Y 옵션을 지정한다
    - blender 의 +Z 가 MGA 의 +Y = forward
    - blender 의 -Y 가 MGA 의 -Z = forward 
  * (바뀔수도 있음) scene 시스템은 사용하지 않는다.

* 프로젝트 여는 방법

project1 solution 을 열면, 다음 4개의 project 가 있음

* MGA : core
* MGAEditor : (거의 진행되지 않음) editor 용 template project.
* Project1 project : engine core 를 만들기위한 demo project
* Project2 project : 실제로 필요한 것을 찾기위해 , toy game 을 만드는 중

최초 1회 실행하면 bin directory 에 config.toml 파일이 생긴다.
여기에서 asset file 들의 path 를 설정해 줘야 한다. <- 나중에 자동화 해야함

즉, project 마다 asset 을 따로 만들지 않아도 된다.

* doc

doc/html/index.html <- 을 열면 doxygen 으로 생성한 문서 있음

* 새 프로젝트 만들기
monogame - desktop GL template 을 이용해서 새 프로젝트 만들기
원하는 곳에 asset folder 만들기 
MGA project 에 참조와 dependency 설정
한번 실행후 생성된 config.toml 에 위에서 만든 asset folder 설정
